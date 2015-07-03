'use strict';

mainApp.directive('highstock', [
	'ChartDataFactory',
	'ConfigFactory',
	'$timeout',
	function(
		ChartDataFactory,
		ConfigFactory,
		$timeout
	) {
		

		return {
			restrict: 'E',
			replace: true,
			template: '<div ng-style="elementStyle"></div>',
			scope: {
				ticker: '@',
				chartName: '@',
				events: '=',
				extremes: '='
			},
			link: function($scope, $element, $attrs) {

				if (angular.isUndefined($attrs.chartName)) {
					throw new Error('Need to set a name for the chart.');
				}

				// The chart so that we can manipulate it in the directive.
				var chart = null;
				var priceHeight = 450;
				var priceTop = 80;
				var indicatorHeight = 180;
				var highstockExtrasHeight = 200;
				var axisPaddingHeight = 10;
				var axisIds = [];

				function addIndicator(data, args) {
					try {
						var seriesName, seriesData, newSeries;

						if (data.plotOnPrice) {
							for (seriesName in data.series) {
								seriesData = data.series[seriesName];
								
								if (seriesData.type === 'flags') {
									newSeries = chart.addSeries({
										name: seriesName,
										data: seriesData.data,
										type: seriesData.type,
										onSeries: 'price-series',
										shape: 'squarepin'
									}, false);
								}
								else {
									newSeries = chart.addSeries({
										name: seriesName,
										data: seriesData.data,
										type: seriesData.type,
										connectNulls: seriesData.connectNulls,
										dashStyle: seriesData.dashStyle
									}, false);
								}
							}
						}
						else {
							var axisId = args.name + '-axis';
							axisIds.push(axisId);

							chart.addAxis({
								id: axisId,
								title: {
									text: args.name
								},
								lineWidth: 2,
								opposite: true,
								offset: 0
							}, false, false);

							for (seriesName in data.series) {
								seriesData = data.series[seriesName];
								newSeries = chart.addSeries({
									name: seriesName,
									data: seriesData.data,
									type: seriesData.type,
									yAxis: axisId
								}, false);
							}
						}

						// Need to rebalance the percentages of the height.
						var heightsAndTops = $scope.setTotalHeightAndGetAxisTopAndHeights(axisIds.length);

						// Update all the heights of each chart.
						for (var i = 0; i < axisIds.length; i++) {
							var axis = chart.get(axisIds[i]);
							axis.update({
								top: heightsAndTops.tops[i],
								height: heightsAndTops.heights[i],
								labels: {
									align: 'center',
									x: -3
								},
							}, false);
						}

					}
					catch (e) {
						console.log(e);
					}
				}

				function createChart(data) {
					axisIds = [
						'price-axis',
						'volume-axis'
					];

					var ohlc = data.price;
					var volume = data.volume;

					var rangeButtons = [{
						type: 'month',
						count: 1,
						text: '1m'
					}, {
						type: 'month',
						count: 3,
						text: '3m'
					}, {
						type: 'month',
						count: 6,
						text: '6m'
					}, {
						type: 'ytd',
						text: 'YTD'
					}, {
						type: 'year',
						count: 1,
						text: '1y'
					}, {
						type: 'all',
						text: 'All'
					}];

					// For mintute data change the range options.
					if (ConfigFactory.getHighStockDataType() !== 'day') {
						rangeButtons = [{
							type: 'minute',
							count: 60,
							text: '1hr'
						}, {
							type: 'minute',
							count: 120,
							text: '2hr'
						}, {
							type: 'day',
							count: 1,
							text: '1day'
						}];						
					}

					var groupingUnits = [[
						ConfigFactory.getHighStockDataType(),
						[ConfigFactory.getNumberOfPointsPerBar()]
					]];

					var highstockConfig = {
						rangeSelector: {
							buttons: rangeButtons,
							selected: 1
						},

						title: {
							text: $scope.ticker
						},

						tooltip: {
							positioner: function () {
								return { x: 10, y: 80 };
							}
						},

						legend: {
							enabled: true,
							align: 'right',
							backgroundColor: '#d9edf7',
							borderColor: '#46b8da',
							borderWidth: 1,
							layout: 'vertical',
							verticalAlign: 'top',
							y: 200,
							shadow: true
						},

						yAxis: [
							{
								labels: {
									align: 'right',
									x: -3
								},
								title: {
									text: 'Candlestick'
								},
								top: priceTop,
								height: priceHeight,
								lineWidth: 2,
								id: axisIds[0]
							},
							{
								labels: {
									align: 'right',
									x: -3
								},
								title: {
									text: 'Volume'
								},
								top: priceHeight + priceTop + axisPaddingHeight,
								height: indicatorHeight,
								offset: 0,
								lineWidth: 2,
								id: axisIds[1]
							}
						],
						series: [
							{
								type: 'candlestick',
								name: $scope.ticker,
								data: ohlc,
								id: 'price-series',
								yAxis: axisIds[0],
								dataGrouping: {
									units: groupingUnits
								}
							},
							{
								type: 'column',
								name: 'Volume',
								id: 'volume-series',
								data: volume,
								yAxis: axisIds[1],
								dataGrouping: {
									units: groupingUnits
								}
							}
						]
					}; // end highstock config

					// Create the chart on the element.
					$element.highcharts('StockChart', highstockConfig);

					// Save for later
					chart = $element.highcharts();

					// Add the events to the chart.
					$scope.$watch('events', function(eventData, oldEventData) {
						if (!eventData) {
							return;
						}

						var newSeries = chart.addSeries({
							data: eventData,
							name: 'Events',
							id: 'event-series',
							type: 'flags',
							onSeries: 'price-series',
							shape: 'squarepin',
							width: 16,
							color: '#ff0000'
						}, false);
					});

					$scope.$watch('extremes', function(newExtremes, oldExtremes) {
						if (newExtremes === oldExtremes) {
							return;
						}

						chart.xAxis[0].setExtremes(newExtremes.min, newExtremes.max);
					});

				}

				$scope.setTotalHeightAndGetAxisTopAndHeights = function(numberOfyAxis) {
					var heights = [];
					var tops = [];
					var extraAxis = numberOfyAxis - 1;
					var totalHeight = priceHeight + highstockExtrasHeight + (extraAxis * (indicatorHeight + axisPaddingHeight));
					var topTotal = priceTop;

					// Percent height for the chart.
					tops.push(topTotal);
					var newHeight = priceHeight
					heights.push(newHeight);
					topTotal += newHeight + axisPaddingHeight;

					for (var i = 0; i < extraAxis; i++) {
						newHeight = indicatorHeight;
						heights.push(newHeight);
						tops.push(topTotal);
						topTotal += newHeight + axisPaddingHeight;
					}

					$scope.elementStyle = {
						'height': totalHeight + 'px'
					};

					return {
						heights: heights,
						tops: tops
					};
				};

				$scope.$on('ClearIndicators', function(msg, args) {
					if (chart) {
						for (var i = chart.series.length - 1; i >= 0; i--) {
							var series = chart.series[i]
							var seriesId = series.options.id;
							if (seriesId && (seriesId === 'price-series' || seriesId === 'volume-series' || seriesId === 'event-series' || seriesId === 'highcharts-navigator-series')) {
								continue;
							}

							// Remove from the chart.
							chart.series[i].remove(false);
						}

						// Remove all the axis that any indicator might have added.
						for (var j = chart.axes.length - 1; j >= 0; j--) {
							var axis = chart.axes[j];
							var axisId = axis.options.id;
							if (axisId) {
								if (axisId !== 'price-axis' && axisId !== 'volume-axis' && axisId !== 'navigator-x-axis' && axisId !== 'navigator-y-axis') {
									axis.remove(false);

									// Remove the id from the saved array so the resizing works.
									for (var k = 0; k < axisIds.length; k++) {
										if (axisIds[k] === axisId) {
											axisIds.splice(k, 1);
											break;
										}
									}
								}
							}
						}
					}
				});

				$scope.$on('AddIndicator', function(msgName, args) {
					// If this is not the chart we are looking for...
					if ($scope.chartName !== args.chartName) {
						return;
					}

					// Get the indicator data and once we have it add it to the chart.
					ChartDataFactory.getIndicatorData(args.name, args.orderId, $scope.ticker).then(function(data) {
						addIndicator(data, args);
					});
				});

				$scope.$on('AddIndicatorExistingData', function(msgName, args) {
					// If this is not the chart we are looking for...
					if ($scope.chartName !== args.chartName) {
						return;
					}

					addIndicator(args.data, args);
				});

				$scope.$on('CreateChartExistingData', function(msgName, args) {
					createChart(args.data);
				});

				$scope.$on('RedrawChart', function(msgName, args) {
					chart.reflow();
					chart.redraw();
				});

				$scope.setTotalHeightAndGetAxisTopAndHeights(2);

				// Get the data first before creating the chart.
				ChartDataFactory.getPriceData($scope.ticker).then(function(data) {
					if (data) {
						createChart(data);
					}
				}); // end $http.get


			}
		};
	}
]);
