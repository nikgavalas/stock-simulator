'use strict';

mainApp.directive('strategyList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/strategy-list.html',
			scope: {
				strategies: '=',
				rowClicked: '='
			},
			controller: [
				'$scope',
				'StrategyListFactory',
				function($scope, StrategyListFactory) {
					// So it can be used in the scope.
					$scope.abs = function(value) {
						return Math.abs(value);
					};

				}
			],
			link: function($scope, $element, $attrs) {
				$scope.showTickers = angular.isDefined($attrs.showTickers);
			}
		};
	}
]);
