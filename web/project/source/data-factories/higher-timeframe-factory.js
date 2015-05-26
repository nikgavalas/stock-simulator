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
		factory.getPriceData = function(date) {
			var deffered = $q.defer();

			$http.get(dataFolder + date + '-data.json').success(function(data) {
				deffered.resolve(data);
			})
			.error(function() {
				deffered.resolve(null);
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
