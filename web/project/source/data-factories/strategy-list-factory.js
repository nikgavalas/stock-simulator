'use strict';

mainApp.factory('StrategyListFactory', [
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
		 * Gets the data for the overall performance of all strategies
		 * @return {Object} Deferred promise
		 */
		factory.getOverallStrategies = function() {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'overall-strategies.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		/**
		 * Gets the data for the overall performance of a selected strategies
		 * @return {Object} Deferred promise
		 */
		factory.getOverallForStrategy = function(strategyName) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'strategies/' + strategyName + '/overall.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		/**
		 * Gets the orders a strategy and symbol.
		 * @return {Object} Promise object for defered use
		 */
		factory.getStrategy = function(strategyName, ticker) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'strategies/' + strategyName + '/' + ticker + '.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
