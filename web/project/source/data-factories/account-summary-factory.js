'use strict';

mainApp.factory('AccountSummaryFactory', [
	function(
	) {
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
		 * Calculates the yearly and monthly returns of all the values added to the class.
		 * @param {Number} year Year of the summary
		 */
		function YearSummary(year) {
			this.year = year;
			this.monthNameToIndex = {};
			this.months = [];
		}

		YearSummary.prototype = {
			/**
			 * Adds a value to be included in the summary.
			 * @param {Date} dateOfValue Date of the account value
			 * @param {Number} value Value of the account on this date
			 */
			addValue: function(dateOfValue, value) {
				var month = parseInt(dateOfValue.getUTCMonth(), 10);
				var monthName = monthNames[dateOfValue.getUTCMonth()];
				
				// Create a new month to hold the values.
				if (angular.isUndefined(this.monthNameToIndex[monthName])) {
					this.months.push({
						name: monthName,
						startDate: dateOfValue,
						startValue: value,
						endDate: dateOfValue,
						endValue: value,
						gain: 0,
						percentGain: 0
					});

					this.monthNameToIndex[monthName] = this.months.length - 1;
				}

				// Calculate the gain if this is the new end of the month.
				var monthIndex = this.monthNameToIndex[monthName];
				var currentMonth = this.months[monthIndex];
				if (dateOfValue > currentMonth.startDate) {
					currentMonth.endDate = dateOfValue;
					currentMonth.endValue = value;
					currentMonth.gain = (currentMonth.endValue - currentMonth.startValue).toFixed(2);
					currentMonth.percentGain = ((currentMonth.gain / currentMonth.startValue) * 100.0).toFixed(2);
				}
			},

			/**
			 * Gets the starting value of the year.
			 * @return {Number} Start value for the year
			 */
			getStartValue: function() {
				return this.months[0].startValue;
			},

			/**
			 * Gets the ending value of the year.
			 * @return {Number} End value for the year
			 */
			getEndValue: function() {
				return this.months[this.months.length - 1].endValue;
			},

			/**
			 * Returns the gain for the year.
			 * @return {Number} The gain for the year
			 */
			getGain: function() {
				return (this.getEndValue() - this.getStartValue()).toFixed(2);
			},

			/**
			 * Returns the percentage gain of the year.
			 * @return {Number} Percent gain for the year
			 */
			getPercentGain: function() {
				return ((this.getGain() / this.getStartValue()) * 100.0).toFixed(2);
			}
		};

		/**
		 * Returns a new year summary object
		 * @param  {Number} year Year of the year summary
		 * @return {Object}      YearSummary object
		 */
		factory.newYearSummary = function(year) {
			return new YearSummary(year);
		};

		return factory;
	}
]);
