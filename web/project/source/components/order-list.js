'use strict';

mainApp.directive('orderList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/order-list.html',
			scope: {
				orders: '=',
				orderClick: '='
			},
			controller: [
				'$scope',
				'$location',
				'ConfigFactory',
				'OrderListFactory',
				function(
					$scope,
					$location,
					ConfigFactory,
					OrderListFactory
				) {

					/**
					 * Expose the absolute function to the scope html
					 * @param  {Number} value Value to return the absolute value for
					 * @return {Number}       Math.abs value
					 */
					$scope.abs = function(value) {
						return Math.abs(value);
					};

				}
			]
		};
	}
]);
