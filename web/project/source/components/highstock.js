'use strict';

mainApp.directive('highstock', [
	'ChartDataFactory',
	function(
		ChartDataFactory
	) {
		
		// The chart so that we can manipulate it in the directive.
		var chart = null;
		var priceHeight = 450;
		var indicatorHeight = 180;
		var axisIds = [];

		return {
			restrict: 'E',
			replace: true,
			template: '<div ng-style="elementStyle"></div>',
			scope: {
				ticker: '@',
				events: '=',
				extremes: '='
			},
			controller: [
				'$scope',
				'$timeout',
				function($scope, $timeout) {

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

					$scope.$on('AddIndicator', function(msgName, args) {
						// Get the indicator data and once we have it add it to the chart.
						ChartDataFactory.getIndicatorData(args.name, $scope.ticker).then(function(data) {
							try {
								var seriesName, seriesData, newSeries;

								if (data.plotOnPrice) {
									for (seriesName in data.series) {
										seriesData = data.series[seriesName];
										newSeries = chart.addSeries({
											name: seriesName,
											data: seriesData.data,
											type: seriesData.type
										}, false);
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
						});
					});

				}
			],
			link: function($scope, $element, $attrs) {

				var heightsAndTops = $scope.setTotalHeightAndGetAxisTopAndHeights(2);

				// Get the data first before creating the chart.
				ChartDataFactory.getPriceData($scope.ticker).then(function(data) {

					axisIds = [
						'price-axis',
						'volume-axis'
					];

					var ohlc = data.price;
					var volume = data.volume;

					var groupingUnits = [[
						'day',
						[1]
					]];

					$element.highcharts('StockChart', {
						rangeSelector: {
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
					}); // end creating highcharts

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

				}); // end $http.get
			}
		};
	}
]);
