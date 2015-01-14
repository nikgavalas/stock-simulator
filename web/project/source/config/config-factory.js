'use strict';

mainApp.factory('ConfigFactory', [
	function() {
		var factory = {};
		
		factory.runName = '';

		factory.setOutputFolder = function(runName) {
			factory.runName = runName;
		};

		factory.getOutputFolder = function() {
			return '/output/' + factory.runName + '/';
		};

		factory.getOutputName = function() {
			return factory.runName;
		};

		return factory;
	}
]);
