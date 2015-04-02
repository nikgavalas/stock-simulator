'use strict';

mainApp.factory('ConfigFactory', [
	function() {
		var factory = {};
		
		var runName = '';
		var simDataType = 'daily';
		var highStockDataType = 'day';
		var numberOfPointsPerBar = 1;

		factory.setOutputFolder = function(name) {
			runName = name;
		};

		factory.getOutputFolder = function() {
			return '/output/' + runName + '/';
		};

		factory.getOutputName = function() {
			return runName;
		};

		factory.setDataType = function(type) {
			var typeToHighstocks = {
				minute: {
					type: 'minute',
					numberOfPoints: 1
				},
				twominute: {
					type: 'minute',
					numberOfPoints: 2
				},
				threeminute: {
					type: 'minute',
					numberOfPoints: 3
				},
				fiveminute: {
					type: 'minute',
					numberOfPoints: 5
				},
				daily: {
					type: 'day',
					numberOfPoints: 1
				}
			};

			simDataType = type;
			highStockDataType = typeToHighstocks[type].type;
			numberOfPointsPerBar = typeToHighstocks[type].numberOfPoints;
		};

		factory.getSimDataType = function() {
			return simDataType;
		};
		
		factory.getHighStockDataType = function() {
			return highStockDataType;
		};

		factory.getNumberOfPointsPerBar = function() {
			return numberOfPointsPerBar;
		};

		factory.getRangeInMilliseconds = function() {
			var typeToMs = {
				minute: 1 * 60 * 1000,
				twominute: 2 * 60 * 1000,
				threeminute: 3 * 60 * 1000,
				fiveminute: 5 * 60 * 1000,
				daily: 24 * 60 * 60 * 1000
			};
			return typeToMs[simDataType];
		};

		return factory;
	}
]);
