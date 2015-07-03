'use strict';


angular.module('mainApp').controller('MainCtrl', [
	'$scope',
	'$routeParams',
	'$location',
	'$window',
	'filterFilter',
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
		filterFilter,
		ConfigFactory,
		OrderListFactory,
		StrategyListFactory,
		TickerListFactory,
		InputOptionsFactory
	) {

		function updateFiltered(term) {
			var originalArray = $scope.showAllTickers ? $scope.tickers : $scope.strategies;
			var filteredArray = filterFilter(originalArray, term);
			if ($scope.showAllTickers) {
				$scope.tickersFiltered = filteredArray;
			}
			else {
				$scope.strategiesFiltered = filteredArray;
			}			
		}

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
		$scope.strategiesFiltered = [];
		StrategyListFactory.getOverallStrategies().then(function(data) {
			$scope.strategies = data;
			// Sort so the strategy with the highest is at the top and that is the one that is shown first.
			$scope.strategies.sort(function(a, b) {
				return b.winPercent - a.winPercent;
			});

			updateFiltered($scope.filterTerm);
		});

		// Load all the overall tickers.
		$scope.tickers = [];
		$scope.tickersFiltered = [];
		TickerListFactory.getOverallTickers().then(function(data) {
			$scope.tickers = data;
			// Sort so the strategy with the highest is at the top and that is the one that is shown first.
			$scope.tickers.sort(function(a, b) {
				return b.gain - a.gain;
			});

			updateFiltered($scope.filterTerm);
		});

		// Load the input parameters used for this run.
		$scope.inputParameters = {};
		InputOptionsFactory.get().then(function(data) {
			$scope.inputParameters = data;
			ConfigFactory.setDataType(data.DataType);
		});

		$scope.$watch('filterTerm', function(term) {
			updateFiltered(term);
		});


		/**
		 * Updates the ticker or strategy list visibility and items
		 * @param  {Boolean} show Show the ticker list or not
		 */
		$scope.updateLists = function(show) {
			$scope.showAllTickers = show;
			updateFiltered($scope.filterTerm);
		};

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
			var url = ConfigFactory.getOutputName() + '/order/' + order.ticker + '/' + order.id + '/' + ConfigFactory.getSimDataType();
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
			var url = ConfigFactory.getOutputName() + '/performance/' + strategy.name + '/' + ConfigFactory.getSimDataType();

			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};

	} // end controller
]);

