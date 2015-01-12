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

		factory.getOverallOrders = function() {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getRunFolder() + 'overall-strategies.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
