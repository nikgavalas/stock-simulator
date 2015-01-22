'use strict';

angular.module('mainApp').controller('StrategyDetailsCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'ConfigFactory',
	'OrderListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		ConfigFactory,
		OrderListFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.strategy = $routeParams.strategy;
		$scope.ticker = $routeParams.ticker;

		$scope.orders = [];
		OrderListFactory.getOrders($scope.strategy, $scope.ticker).then(function(data) {
			$scope.orders = data.orders;

			// Add all the indicators to the chart.
			for (var i = 0; i < data.indicators.length; i++) {
				$scope.$broadcast('AddIndicator', { name: data.indicators[i] });
			}
		});


		/**
		 * Snap the chart to the order location.
		 * @param  {Object} order Order that was clicked
		 */
		$scope.orderClick = function(order) {
			console.log('Snap the chart to the order location');
		};

	} // end controller
]);

