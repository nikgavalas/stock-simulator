'use strict';

mainApp.directive('orderList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/order-list.html',
			scope: {
				orderData: '='
			},
			controller: [
				'$scope',
				'OrderListFactory',
				function($scope, OrderListFactory) {
	
					$scope.orders = [];

					OrderListFactory.getOverallOrders().then(function(data) {
						$scope.orders = data;
					});

					$scope.orderClicked = function(order) {
						console.log(order);
					};
				}
			]
		};
	}
]);
