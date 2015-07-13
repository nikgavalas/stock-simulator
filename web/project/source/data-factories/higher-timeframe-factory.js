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
		 * @param {String} date String date
		 * @param {String} ticker String of the ticker for the data
		 * @return {Object} Deferred promise
		 */
		factory.get = function(date, ticker) {
			var deffered = $q.defer();

			var folderName = dataFolder + ticker + '/';
			var dataPromise = $http.get(folderName + date + '-data.json');
			var indicatorPromise = $http.get(folderName + date + '-ind.json');
			var statesPromise = $http.get(folderName + date + '-states.json');

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
