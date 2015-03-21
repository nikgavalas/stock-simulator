'use strict';

mainApp.factory('InputOptionsFactory', [
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
		 * Gets the the input parameters used to run this sim
		 * @return {Object} Deferred promise
		 */
		factory.get = function(date) {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'input.json').success(function(data) {
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
