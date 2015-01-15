'use strict';

angular.module('mainApp').controller('StrategyDetailsCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'ConfigFactory',
	function(
		$scope,
		$routeParams,
		$location,
		ConfigFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.strategy = $routeParams.strategy;
		$scope.ticker = $routeParams.ticker;

		// TODO: get from data
		$scope.orders = {
			'1': {
				'ticker': 'AAPL',
				'id': 1,
				'buyPrice': 300,
				'sellPrice': 400,
				'numShares': 43,
				'gain': 443.44
			},
			'2': {
				'ticker': 'APPL',
				'id': 2,
				'buyPrice': 300,
				'sellPrice': 400,
				'numShares': 43,
				'gain': -443.44
			},
			'3': {
				'ticker': 'APPL',
				'id': 3,
				'buyPrice': 300,
				'sellPrice': 400,
				'numShares': 43,
				'gain': 443.44
			}
		};

		/**
		 * Snap the chart to the order location.
		 * @param  {Object} order Order that was clicked
		 */
		$scope.orderClick = function(order) {
			console.log('Snap the chart to the order location');
		};

	} // end controller
]);

