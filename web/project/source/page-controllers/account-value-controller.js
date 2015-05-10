'use strict';

angular.module('mainApp').controller('AccountValueCtrl', [
	'$scope',
	'$routeParams',
	'$http',
	'$q',
	'ConfigFactory',
	'AccountSummaryFactory',
	'OrderListFactory',
	function(
		$scope,
		$routeParams,
		$http,
		$q,
		ConfigFactory,
		AccountSummaryFactory,
		OrderListFactory
	) {

		var orderSummary = null;
		var yearSummaries = {};
		var ordersPromise = $q.defer();
		var accountPromise = $q.defer();

		/**
		 * Makes it easier to access the month summaries.
		 * @param {Object} summary Month summary object created by OrderListFactory
		 * @class
		 */
		function MonthSummary(summary) {
			this.summary = summary;
		}

		/**
		 * Returns the number of orders for a month.
		 * @returns {Number} see description
		 */
		MonthSummary.prototype.getNumOrders = function() {
			return this.summary ? this.summary.numberOfOrders : 0;
		};

		/**
		 * Returns the win percent for the month.
		 * @returns {Number} see description
		 */
		MonthSummary.prototype.getWinPercent = function() {
			return this.summary ? this.summary.winPercent : 0.00;
		};

		/**
		 * Returns the loss percent for the month.
		 * @returns {Number} see description
		 */
		MonthSummary.prototype.getLossPercent = function() {
			return this.summary ? this.summary.lossPercent : 0.00;
		};

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		OrderListFactory.getOverallOrders().then(function(orderData) {
			var orders = orderData;
			orderSummary = OrderListFactory.getOrdersSummary(orders);
			ordersPromise.resolve();
		});

		$http.get(ConfigFactory.getOutputFolder() + 'overall-account.json').success(function(data) {
			$scope.chartData = data.accountValue;

			var currentYear = new Date($scope.chartData[0][0]).getUTCFullYear();
			var currentSummary = AccountSummaryFactory.newYearSummary(currentYear);
			yearSummaries[currentYear] = currentSummary;

			for (var i = 0; i < $scope.chartData.length; i++) {
				var timeAndValue = $scope.chartData[i];
				var dateOfItem = new Date(timeAndValue[0]);

				// Move to the next year if we need to.
				if (dateOfItem.getUTCFullYear() !== currentYear) {
					currentYear = dateOfItem.getUTCFullYear();
					currentSummary = AccountSummaryFactory.newYearSummary(currentYear);
					yearSummaries[currentYear] = currentSummary;
				}

				// Save the value for calculations.
				currentSummary.addValue(dateOfItem, timeAndValue[1]);
			}

			accountPromise.resolve();
		});

		// Wait for both server requests.
		var allRequests = $q.all([
			ordersPromise.promise,
			accountPromise.promise
		]);

		allRequests.then(function() {
			$scope.yearSummaries = yearSummaries;
		});

		/**
		 * Returns the month summary wrapper object for a given year and month.
		 * @param   {Number} year  Year number
		 * @param   {String} month Month string name from DateFactory
		 * @returns {MonthSummary} see description
		 */
		$scope.getMonthOrderSummary = function(year, month) {
			var monthSummary = new MonthSummary();
			if (orderSummary && orderSummary.years[year]) {
				monthSummary = new MonthSummary(orderSummary.years[year].months[month]);
			}

			return monthSummary;
		};
	} // end controller
]);

