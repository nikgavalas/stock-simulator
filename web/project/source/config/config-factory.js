'use strict';

mainApp.factory('ConfigFactory', [
	function() {
		var factory = {};
		
		factory.runName = '';

		factory.setRunName = function(runName) {
			factory.runName = runName;
		};

		factory.getRunFolder = function() {
			return '/output/' + factory.runName + '/';
		};

		return factory;
	}
]);
