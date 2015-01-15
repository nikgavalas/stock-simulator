'use strict';

mainApp.factory('OrderListFactory', [
	'$http',
	'$q',
	'ConfigFactory',
	function(
		$http,
		$q,
		ConfigFactory
	) {
		var factory = {};

		/**
		 * Gets the orders for the overall strategy
		 * @return {Object} Promise object for defered use
		 */
		factory.getOverallOrders = function() {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'overall-orders.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		/**
		 * Gets the orders a strategy and symbol.
		 * @return {Object} Promise object for defered use
		 */
		factory.getOrders = function(strategyName, ticker) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'strategies/' + strategyName + '/' + ticker + '.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
