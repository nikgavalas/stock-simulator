'use strict';


angular.module('mainApp').controller('MainCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'ConfigFactory',
	'StrategyListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		ConfigFactory,
		StrategyListFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		// Load all the overall orders.
		$scope.strategies = [];
		StrategyListFactory.getOverallOrders().then(function(data) {
			$scope.strategies = data;
		});


		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy) {
			$location.url(ConfigFactory.getOutputName() + '/performance/' + strategy.name);
		};

	} // end controller
]);

