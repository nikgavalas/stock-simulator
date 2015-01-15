'use strict';

angular.module('mainApp').controller('PerformanceCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'ConfigFactory',
	function(
		$scope,
		$routeParams,
		$location,
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
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy) {
			$location.url(ConfigFactory.getOutputName() + '/strategy/' + $scope.strategy + '/' + strategy.ticker);
		};

	} // end controller
]);

