'use strict';

angular.module('mainApp').controller('PerformanceCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'$window',
	'ConfigFactory',
	'StrategyListFactory',
	function(
		$scope,
		$routeParams,
		$location,
		$window,
		ConfigFactory,
		StrategyListFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.strategy = $routeParams.strategy;

		// Load all the overall strategies.
		$scope.strategies = [];
		StrategyListFactory.getOverallForStrategy($scope.strategy).then(function(data) {
			$scope.strategies = data;
		});

		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy, $event) {
			var url = ConfigFactory.getOutputName() + '/strategy/' + $scope.strategy + '/' + strategy.name;

			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

	} // end controller
]);

