'use strict';

angular.module('mainApp').controller('HigherTimeframeCtrl', [
	'$scope',
	'HigherTimeframeFactory',
	function(
		$scope,
		HigherTimeframeFactory
	) {

		$scope.ticker = $routeParams.ticker;


		HigherTimeframeFactory.getPriceData().then(function(data) {
					// Add all the indicators to the chart.
					for (var i = 0; i < data.indicators.length; i++) {
						$scope.$broadcast('AddIndicator', { name: data.indicators[i], chartName: 'lowerTimeframe' });
					}
				});
			}
		});


		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy, $event) {
			var url = ConfigFactory.getOutputName() + '/strategy/' + strategy.name + '/' + orderData.ticker + '/' + ConfigFactory.getSimDataType();

			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

	} // end controller
]);

