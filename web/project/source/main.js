'use strict';


angular.module('mainApp').controller('MainCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'$window',
	'ConfigFactory',
	'OrderListFactory',
	'StrategyListFactory',
	'TickerListFactory',
	'InputOptionsFactory',
	function(
		$scope,
		$routeParams,
		$location,
		$window,
		ConfigFactory,
		OrderListFactory,
		StrategyListFactory,
		TickerListFactory,
		InputOptionsFactory
	) {
		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		// TODO: have this toggled with a button group.
		$scope.showAllTickers = true;

		$scope.orders = [];
		OrderListFactory.getOverallOrders().then(function(data) {
			$scope.orders = data;
		});

		// Load all the overall strategies.
		$scope.strategies = [];
		StrategyListFactory.getOverallStrategies().then(function(data) {
			$scope.strategies = data;
			// Sort so the strategy with the highest is at the top and that is the one that is shown first.
			$scope.strategies.sort(function(a, b) {
				return b.profitTargetPercent - a.profitTargetPercent;
			});
		});

		// Load all the overall tickers.
		$scope.tickers = [];
		TickerListFactory.getOverallTickers().then(function(data) {
			$scope.tickers = data;
			// Sort so the strategy with the highest is at the top and that is the one that is shown first.
			$scope.tickers.sort(function(a, b) {
				return b.profitTargetPercent - a.profitTargetPercent;
			});
		});

		// Load the input parameters used for this run.
		$scope.inputParameters = {};
		InputOptionsFactory.get().then(function(data) {
			$scope.inputParameters = data;
		});

		/**
		 * Goto a location. TODO: rewrite
		 * @param  {[type]} order [description]
		 * @return {[type]}       [description]
		 */
		$scope.orderClick = function(order, $event) {
			// TODO: Maybe show the order here on the charts instead. So when clicked
			// Change chart to ticker
			// Change to strategy that was used to buy this order and display stats
			// Change indicators to match the strategy
			// Show buy and sell locations on the chart
			var url = ConfigFactory.getOutputName() + '/order/' + order.ticker + '/' + order.id;
			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

		/**
		 * Called when the user clicks on a row in the strategy table
		 * @param  {Object} strategy The strategy row that was clicked
		 */
		$scope.strategyClick = function(strategy, $event) {
			var url = ConfigFactory.getOutputName() + '/performance/' + strategy.name;

			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

	} // end controller
]);

