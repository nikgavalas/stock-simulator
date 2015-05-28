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

		/**
		 * Converts and returns a date object to a file frendly string for loading in our program.
		 * @param   {Date} date Date object to convert
		 * @returns {String} see description
		 */
		factory.convertDateToString = function(date) {
			var month = (date.getUTCMonth() + 1).toString();
			if (month.length < 2) {
				month = '0' + month;
			}
			var day = (date.getUTCDate()).toString();
			if (day.length < 2) {
				day = '0' + day;
			}
			return date.getUTCFullYear() + '-' + month + '-' + day;
		};

		/**
		 * Returns true if this is a valid date.
		 * @param   {Object}  d Object to check if it's a valid date
		 * @returns {Boolean} see description
		 */
		factory.isValidDate = function(d) {
			return Object.prototype.toString.call(d) === '[object Date]' && !isNaN(d.getTime());
		};

		return factory;
	}
]);
