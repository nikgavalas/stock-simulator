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
				var indicatorHeight = 180;
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

							chart.redraw();
							chart.reflow();
						}
						else {
							var axisId = args.name + '-axis';
							axisIds.push(axisId);

							chart.addAxis({
								id: axisId,
								labels: {
									align: 'right',
									x: -3
								},
								title: {
									text: args.name
								},
								top: '15%',
								height: '15%',
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

							$timeout(function() {
								chart.redraw();
								chart.reflow();
							}, 500);
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
								top: heightsAndTops.tops[0],
								height: heightsAndTops.heights[0],
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
								top: heightsAndTops.tops[1],
								height: heightsAndTops.heights[1],
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
					var totalHeight = priceHeight + (extraAxis * indicatorHeight);
					var topTotal = 0;
					// Percent height for the chart.
					tops.push(topTotal + '%');
					var newHeight = (priceHeight / totalHeight) * 100;
					heights.push(newHeight + '%');
					topTotal += newHeight;

					for (var i = 0; i < extraAxis; i++) {
						newHeight = (indicatorHeight / totalHeight) * 100;
						heights.push(newHeight + '%');
						tops.push(topTotal + '%');
						topTotal += newHeight;
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
					while (chart && chart.series.length > 4) {
						chart.series[4].remove(false);
					}

					if (args && args.redraw) {
						$timeout(function() {
							chart.redraw();
							chart.reflow();
						}, 500);
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

				var heightsAndTops = $scope.setTotalHeightAndGetAxisTopAndHeights(2);

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
