'use strict';

mainApp.directive('strategyList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/strategy-list.html',
			scope: {
				orderData: '='
			},
			controller: [
				'$scope',
				'StrategyListFactory',
				function($scope, StrategyListFactory) {
	
					$scope.strategies = [];

					StrategyListFactory.getOverallOrders().then(function(data) {
						$scope.strategies = data;
					});

					$scope.orderClicked = function(order) {
						console.log(order);
					};
				}
			]
		};
	}
]);
