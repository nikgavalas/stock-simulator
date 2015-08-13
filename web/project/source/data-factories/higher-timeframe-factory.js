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
			var statesPromise = $http.get(folderName + date + '-states.json');
			var indicatorPromiseDtOsc = $http.get(folderName + date + '-ind0.json');
			var indicatorPromiseAtr = $http.get(folderName + date + '-ind1.json');
			var indicatorPromiseKeltner = $http.get(folderName + date + '-ind2.json');
			var indicatorPromiseSma = $http.get(folderName + date + '-ind3.json');

			// Wait for both requests to complete.
			$q.all([dataPromise, statesPromise, indicatorPromiseDtOsc, indicatorPromiseAtr, indicatorPromiseKeltner, indicatorPromiseSma]).then(function(data) {
				deffered.resolve({
					priceData: data[0].data,
					statesData: data[1].data,
					indicatorData: [
						data[2].data,
						data[3].data,
						data[4].data,
						data[5].data
					]
				});
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
