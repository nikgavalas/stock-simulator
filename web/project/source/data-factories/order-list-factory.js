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
