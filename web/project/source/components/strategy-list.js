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
					/**
					 * Expose the absolute function to the scope html
					 * @param  {Number} value Value to return the absolute value for
					 * @return {Number}       Math.abs value
					 */
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
