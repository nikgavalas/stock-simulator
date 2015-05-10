'use strict';

mainApp.factory('OrderListFactory', [
	'$http',
	'$q',
	'ConfigFactory',
	'DateFactory',
	function(
		$http,
		$q,
		ConfigFactory,
		DateFactory
	) {

		var factory = {};

		/**
		 * Holds all the order stats sorted by year and month.
		 * @class
		 */
		function OrderSummary() {
		}

		/**
		 * Calculates the stats for every month based on its orders.
		 */
		OrderSummary.prototype.calculateStats = function() {
			for (var yearKey in this.years) {
				var year = this.years[yearKey];

				// TODO: Aggregate all the month stats and save them for the year here.
				// Then output those in the account value areas.

				for (var monthKey in year.months) {
					var month = year.months[monthKey];

					month.numberOfOrders = month.orders.length;

					var totalWins = 0;
					var totalLosses = 0;
					for (var i = 0; i < month.orders.length; i++) {
						if (month.orders[i].gain >= 0) {
							++totalWins;
						}
						else {
							++totalLosses;
						}
					}

					month.winPercent = month.numberOfOrders > 0 ? ((totalWins / month.numberOfOrders) * 100).toFixed(2) : 0;
					month.lossPercent = month.numberOfOrders > 0 ? ((totalLosses / month.numberOfOrders) * 100).toFixed(2) : 0;
				}
			}
		};

		/**
		 * Initializes all the years and months with all the orders.
		 * @param   {Object[]} allOrders All the orders to sort
		 */
		OrderSummary.prototype.initialize = function(allOrders) {
			this.years = {};

			for (var i = 0; i < allOrders.length; i++) {
				var order = allOrders[i];
				var sellDate = new Date(order.sellDate);
				var sellYear = sellDate.getUTCFullYear();
				var sellMonth = DateFactory.getMonthName(sellDate.getUTCMonth());

				// Create the year if it doesn't exist.
				if (!this.years[sellYear]) {
					this.years[sellYear] = {
						months: {}
					};
				}

				var year = this.years[sellYear];

				// Create the month if it doesn't exist.
				if (!year.months[sellMonth]) {
					year.months[sellMonth] = {
						orders: []
					};
				}

				// Now that we have both a year and a month. We add this order.
				year.months[sellMonth].orders.push(order);
			}

			// At this point, all the orders are sorted into years and months. So loop
			// through all and calculate the stats.
			this.calculateStats();
		};

		/**
		 * Returns an object that has all the order's stats stored by year and month.
		 * @param   {Object[]} allOrders Array of all the orders placed.
		 * @returns {OrderSummary} see description
		 */
		factory.getOrdersSummary = function(allOrders) {
			var orderSum = new OrderSummary();
			orderSum.initialize(allOrders);
			return orderSum;
		};

		/**
		 * Gets the orders for the overall strategy
		 * @return {Object} Promise object for defered use
		 */
		factory.getOverallOrders = function() {
			var deffered = $q.defer();

			$http.get(ConfigFactory.getOutputFolder() + 'overall-orders.json').success(function(data) {
				deffered.resolve(data);
			});

			return deffered.promise;
		};		

		/**
		 * Converts all orders to an array of events to use with highstocks
		 * @param  {Object} orders The orders from the data
		 * @return {Array}        Array of events
		 */
		factory.convertOrdersToDataSeries = function(orders) {
			var events = [];
			for (var orderId in orders) {
				var order = orders[orderId];
				var buyDate = Date.parse(order.buyDate);
				var sellDate = Date.parse(order.sellDate);

				var buyEvent = {
					x: buyDate,
					title: 'B',
					text: 'Bought ' + order.numShares + ' shares at ' + order.buyPrice
				};

				var sellEvent = {
					x: sellDate,
					title: 'S',
					text: 'Sold ' + order.numShares + ' shares at ' + order.sellPrice
				};

				events.push(buyEvent);
				events.push(sellEvent);
			}

			// Highcharts wants the events sorted by date. So sort by the x value
			events.sort(function(a, b) {
				return a.x - b.x;
			});
			
			return events;
		};

		return factory;
	}
]);
