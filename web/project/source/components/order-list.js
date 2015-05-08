'use strict';

mainApp.directive('orderList', [
	function() {
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'source/components/order-list.html',
			scope: {
				orders: '=',
				orderClick: '=',
				activeOrder: '='
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

					var statusNames = {
						'4': 'Length',
						'3': 'Stop',
						'2': 'Profit'
					};

					/**
					 * Expose the absolute function to the scope html
					 * @param  {Number} value Value to return the absolute value for
					 * @return {Number}       Math.abs value
					 */
					$scope.abs = function(value) {
						return Math.abs(value);
					};

					/**
					 * Return the name for the order status value.
					 * @param  {Number} value Order status enum value
					 * @return {String}       String of the order status value
					 */
					$scope.statusName = function(value) {
						return statusNames[value];
					};
				}
			],
			link: function($scope, $element, $attrs) {
				$scope.numberOrdersPerPage = 20;
				$scope.currentPage = 1;

				$scope.getPageOrders = function() {
					var startIndex = ($scope.currentPage - 1) * $scope.numberOrdersPerPage;
					var endIndex = $scope.currentPage * $scope.numberOrdersPerPage;
					if (endIndex > $scope.orders.length) {
						endIndex = $scope.orders.length;
					}

					return $scope.orders.slice(startIndex, endIndex);
				};

				/**
				 * Returns the order gain as a percent.
				 * @param   {Object} order Order to calculate from
				 * @returns {Number} see description
				 */
				$scope.getPercentGain = function(order) {
					var percent = (((order.sellPrice - order.buyPrice) / order.buyPrice) * 100).toFixed(2);
					return order.orderType === 0 ? percent : -percent;
				};
			}
		};
	}
]);
