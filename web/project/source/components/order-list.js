'use strict';

mainApp.directive('orderList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/order-list.html',
			scope: {
				strategyName: '@'
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
	
					$scope.orders = [];

					OrderListFactory.getOverallOrders().then(function(data) {
						$scope.orders = data;
					});

					/**
					 * Goto a location. TODO: rewrite
					 * @param  {[type]} order [description]
					 * @return {[type]}       [description]
					 */
					$scope.orderClick = function(order) {
						$location.url(ConfigFactory.getOutputName() + '/order/' + order.ticker + '/' + order.id);
					};

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
