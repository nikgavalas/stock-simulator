'use strict';

angular.module('mainApp').controller('OrderDetailsCtrl', [
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
		var orderData = null;

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.ticker = $routeParams.ticker;
		$scope.orderId = $routeParams.orderId;

		// TODO: get from data.
		$scope.orderDate = '12/22/2014';

		// Load all the overall strategies.
		$scope.strategies = [];

		OrderListFactory.getOverallOrders().then(function(data) {
			orderData = data[$scope.orderId];
			$scope.strategies = orderData.strategies;
		});


		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy) {
			$location.url(ConfigFactory.getOutputName() + '/strategy/' + strategy.name + '/' + orderData.ticker);
		};

	} // end controller
]);

