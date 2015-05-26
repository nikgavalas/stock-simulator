'use strict';

mainApp.factory('HigherTimeframeFactory', [
	'$http',
	'$q',
	'ConfigFactory',
	function(
		$http,
		$q,
		ConfigFactory
	) {
		var factory = {};

		var dataFolder = '/output/higher/';

		/**
		 * Gets the price data of the higher timeframe.
		 * @return {Object} Deferred promise
		 */
		factory.get = function(date) {
			var deffered = $q.defer();

			var dataPromise = $http.get(dataFolder + date + '-data.json');
			var indicatorPromise = $http.get(dataFolder + date + '-ind.json');
			var statesPromise = $http.get(dataFolder + date + '-states.json');

			// Wait for both requests to complete.
			$q.all([dataPromise, indicatorPromise, statesPromise]).then(function(data) {
				deffered.resolve({
					priceData: data[0].data,
					indicatorData: data[1].data,
					statesData: data[2].data
				});
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
