'use strict';

mainApp.directive('highstock', [
	'ChartDataFactory',
	function(
		ChartDataFactory
	) {
		return {
			restrict: 'E',
			replace: true,
			template: '<div ng-style="elementStyle"></div>',
			scope: {
				ticker: '@'
			},
			// controller: [
			// 	'$scope',
			// 	function($scope) {

			// 	}
			// ],
			link: function($scope, $element, $attrs) {

				$scope.elementStyle = {
					'height': '600px'
				};

				// Get the data first before creating the chart.
				ChartDataFactory.getPriceData($scope.ticker).then(function(data) {

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

						yAxis: [{
								labels: {
										align: 'right',
										x: -3
								},
								title: {
										text: 'Candlestick'
								},
								height: '75%',
								lineWidth: 2
						}, {
								labels: {
										align: 'right',
										x: -3
								},
								title: {
										text: 'Volume'
								},
								top: '80%',
								height: '15%',
								offset: 0,
								lineWidth: 2
						}],

						series: [
							{
									type: 'candlestick',
									name: $scope.ticker,
									data: ohlc,
									dataGrouping: {
											units: groupingUnits
									}
							},
							{
								type: 'column',
								name: 'Volume',
								data: volume,
								yAxis: 1,
								dataGrouping: {
										units: groupingUnits
								}
							}
						]
					}); // end creating highcharts

				}); // end $http.get
			}
		};
	}
]);
