'use strict';

mainApp.directive('strategyList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/strategy-list.html',
			scope: {
				strategies: '=',
				rowClicked: '=',
				selected: '@'
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
				$scope.numberItemsPerPage = 20;
				$scope.currentPage = 1;
				$scope.pageItems = [];

				function updatePageItems() {
					var startIndex = ($scope.currentPage - 1) * $scope.numberItemsPerPage;
					var endIndex = $scope.currentPage * $scope.numberItemsPerPage;
					if (endIndex > $scope.strategies.length) {
						endIndex = $scope.strategies.length;
					}

					$scope.pageItems = $scope.strategies.slice(startIndex, endIndex);
				}

				$scope.$watch('currentPage', function() {
					updatePageItems();
				});

				$scope.$watch('strategies', function() {
					updatePageItems();
				});
			}
		};
	}
]);
