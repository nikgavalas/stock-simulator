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

		factory.getOverallOrders = function() {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'overall-orders.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		return factory;
	}
]);
