'use strict';


angular.module('mainApp').controller('MainCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'ConfigFactory',
	'OrderListFactory',
	'StrategyListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		ConfigFactory,
		OrderListFactory,
		StrategyListFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.orders = [];
		OrderListFactory.getOverallOrders().then(function(data) {
			$scope.orders = data;
		});

		// Load all the overall strategies.
		$scope.strategies = [];
		StrategyListFactory.getOverallStrategies().then(function(data) {
			$scope.strategies = data;
		});

		/**
		 * Goto a location. TODO: rewrite
		 * @param  {[type]} order [description]
		 * @return {[type]}       [description]
		 */
		$scope.orderClick = function(order) {
			// TODO: Maybe show the order here on the charts instead. So when clicked
			// Change chart to ticker
			// Change to strategy that was used to buy this order and display stats
			// Change indicators to match the strategy
			// Show buy and sell locations on the chart
			$location.url(ConfigFactory.getOutputName() + '/order/' + order.ticker + '/' + order.id);
		};

		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy) {
			$location.url(ConfigFactory.getOutputName() + '/performance/' + strategy.name);
		};

	} // end controller
]);

