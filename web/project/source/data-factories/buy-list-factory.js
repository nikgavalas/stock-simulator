'use strict';

mainApp.factory('BuyListFactory', [
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
		 * Gets the buy list for a date
		 * @return {Object} Deferred promise
		 */
		factory.getBuyList = function(date) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'buylist/' + date + '.json').success(function(data) {
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
