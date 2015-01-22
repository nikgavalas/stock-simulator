'use strict';

mainApp.factory('ChartDataFactory', [
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
		 * Returns the candlestick and volume data for the ticker name.
		 * @return {Object} Promise object for defered use
		 */
		factory.getPriceData = function(ticker) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'pricedata/' + ticker + '.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		/**
		 * Returns the indicator data for the ticker and indicator.
		 * @return {Object} Promise object for defered use
		 */
		factory.getIndicatorData = function(indicatorName, ticker) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'indicators/' + indicatorName + '/' + ticker + '.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		
		return factory;
	}
]);
