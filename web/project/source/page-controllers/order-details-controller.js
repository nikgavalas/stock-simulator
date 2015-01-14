'use strict';

angular.module('mainApp').controller('OrderDetailsCtrl', [
	'$scope',
	'$routeParams',
	'ConfigFactory',
	function(
		$scope,
		$routeParams,
		ConfigFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.ticker = $routeParams.ticker;
		$scope.orderId = $routeParams.orderId;

		// TODO: get from data.
		$scope.orderDate = '12/22/2014';

	} // end controller
]);

