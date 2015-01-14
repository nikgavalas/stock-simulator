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

					// Goto the order details for this order.
					$scope.orderClick = function(order) {
						$location.url(ConfigFactory.getOutputName() + '/order/' + order.ticker + '/' + order.id);
					};

				}
			]
		};
	}
]);
