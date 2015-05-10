'use strict';

mainApp.factory('DateFactory', [
	function() {

		var factory = {};

		var monthNames = [
			'Jan',
			'Feb',
			'Mar',
			'Apr',
			'May',
			'Jun',
			'Jul',
			'Aug',
			'Sep',
			'Oct',
			'Nov',
			'Dec'
		];

		/**
		 * Returns the month name of from the 0 indexed month index.
		 * @param   {Number} monthIndex 0 Indexed month
		 * @returns {String} see description
		 */
		factory.getMonthName = function(monthIndex) {
			return monthNames[monthIndex];
		};

		return factory;
	}
]);
