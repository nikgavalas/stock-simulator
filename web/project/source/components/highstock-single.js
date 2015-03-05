'use strict';

mainApp.directive('highstockSingle', [
	function() {
		
		// The chart so that we can manipulate it in the directive.
		var chart = null;
		var priceHeight = 450;
		var indicatorHeight = 180;
		var axisIds = [];

		return {
			restrict: 'E',
			replace: true,
			template: '<div style="height: 500px;"></div>',
			scope: {
				chartData: '=',
			},
			link: function($scope, $element, $attrs) {

				var groupingUnits = [[
					'day',
					[1]
				]];

				$scope.$watch('chartData', function() {
					// Create the chart
					$element.highcharts('StockChart', {
						rangeSelector : {
							selected : 1
						},

						title : {
							text : 'Overall Account Value'
						},

						series : [{
							name : 'Account Value',
							data : $scope.chartData,
							tooltip: {
								valueDecimals: 2
							}
						}]
					});
				});

					// $element.highcharts('StockChart', {
					// 	rangeSelector: {
					// 			selected: 1
					// 	},

					// 	title: {
					// 			text: $scope.ticker
					// 	},

					// 	tooltip: {
					// 		positioner: function () {
					// 			return { x: 10, y: 80 };
					// 		}
					// 	},

					// 	legend: {
					// 		enabled: true,
					// 		align: 'right',
					// 		backgroundColor: '#d9edf7',
					// 		borderColor: '#46b8da',
					// 		borderWidth: 1,
					// 		layout: 'vertical',
					// 		verticalAlign: 'top',
					// 		y: 200,
					// 		shadow: true
					// 	},

					// 	yAxis: [
					// 		{
					// 			labels: {
					// 				align: 'right',
					// 				x: -3
					// 			},
					// 			title: {
					// 				text: 'Candlestick'
					// 			},
					// 			top: heightsAndTops.tops[0],
					// 			height: heightsAndTops.heights[0],
					// 			lineWidth: 2,
					// 			id: axisIds[0]
					// 		},
					// 		{
					// 			labels: {
					// 				align: 'right',
					// 				x: -3
					// 			},
					// 			title: {
					// 				text: 'Volume'
					// 			},
					// 			top: heightsAndTops.tops[1],
					// 			height: heightsAndTops.heights[1],
					// 			offset: 0,
					// 			lineWidth: 2,
					// 			id: axisIds[1]
					// 		}
					// 	],
					// 	series: [
					// 		{
					// 			type: 'candlestick',
					// 			name: $scope.ticker,
					// 			data: ohlc,
					// 			id: 'price-series',
					// 			yAxis: axisIds[0],
					// 			dataGrouping: {
					// 				units: groupingUnits
					// 			}
					// 		},
					// 		{
					// 			type: 'column',
					// 			name: 'Volume',
					// 			data: volume,
					// 			yAxis: axisIds[1],
					// 			dataGrouping: {
					// 				units: groupingUnits
					// 			}
					// 		}
					// 	]
					// }); // end creating highcharts

			}
		};
	}
]);
