'use strict';

angular.module('mainApp').controller('PerformanceCtrl', [
	'$scope',
	'$routeParams',
	'ConfigFactory',
	function(
		$scope,
		$routeParams,
		ConfigFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.strategy = $routeParams.strategy;

		// TODO: get from data
		$scope.strategies = [
			{
				'ticker': 'AAPL',
				'winPercent': 44,
				'lossPercent': 56,
				'gain': -44.55
			},
			{
				'ticker': 'AMD',
				'winPercent': 80,
				'lossPercent': 20,
				'gain': 443.44
			},
			{
				'ticker': 'INTC',
				'winPercent': 60,
				'lossPercent': 40,
				'gain': 1000
			}
		];

		/**
		 * Called when the user clicks on a row in the table.
		 * @param  {Object} strategy Strategy object click
		 */
		$scope.strategyClick = function(strategy) {
			console.log(strategy);
		};

	} // end controller
]);

