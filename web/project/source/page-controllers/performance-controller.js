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
		ConfigFactory.setDataType($routeParams.type);

		$scope.strategy = $routeParams.strategy;

		// Load all the overall strategies.
		$scope.strategies = [];
		StrategyListFactory.getOverallForStrategy($scope.strategy).then(function(data) {
			$scope.strategies = data;
			// Sort so the strategy with the highest is at the top and that is the one that is shown first.
			$scope.strategies.sort(function(a, b) {
				return b.profitTargetPercent - a.profitTargetPercent;
			});
		});

		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy, $event) {
			var url = ConfigFactory.getOutputName() + '/strategy/' + $scope.strategy + '/' + strategy.name + '/' + ConfigFactory.getSimDataType();

			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

	} // end controller
]);

