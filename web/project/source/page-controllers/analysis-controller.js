'use strict';

angular.module('mainApp').controller('AnalysisCtrl', [
	'$scope',
	'$routeParams',
	'ConfigFactory',
	'OrderListFactory',
	function(
		$scope,
		$routeParams,
		ConfigFactory,
		OrderListFactory
	) {

		// Save since it will be used in the rest of the app.
		ConfigFactory.setOutputFolder($routeParams.runName);

		$scope.allOrders = [];
		$scope.filteredOrders = [];
		$scope.extraColumns = {};
		$scope.numHighlighted = 0;
		$scope.numHighlightedText = 'Highlighted 0 of 0 orders (0%)';
		$scope.errorMsg = '';

		OrderListFactory.getOverallOrders().then(function(data) {
			$scope.allOrders = data;
		});

		/**
		 * Updates the list of orders with the filters and highlights.
		 */
		$scope.updateList = function() {
			$scope.errorMsg = '';
			
			try {
				$scope.numHighlightedText = 'Highlighted 0 of 0 orders (0%)';
				if ($scope.allOrders.length) {
					$scope.filteredOrders = $scope.allOrders.filter(new Function('o', 'return ' + $scope.inputFilterBy));

					// All the orders are the same so we can just use the first one for the column names.
					if ($scope.filteredOrders.length) {
						$scope.extraColumns = $scope.filteredOrders[0].extra;

						// Manually have to loop through the list to add the highlighted property.
						$scope.numHighlighted = 0;
						var highlightCompareFn = new Function('o', 'return ' + $scope.inputHighlightBy);
						for (var i = 0; i < $scope.filteredOrders.length; i++) {
							$scope.filteredOrders[i].isHighlighted = $scope.inputHighlightBy && highlightCompareFn($scope.filteredOrders[i]);
							if ($scope.filteredOrders[i].isHighlighted) {
								++$scope.numHighlighted;
							}
						}

						$scope.numHighlightedText = 'Highlighted ' + $scope.numHighlighted + ' of ' + $scope.filteredOrders.length + 
							' orders (' + ($scope.filteredOrders.length > 0 ? (($scope.numHighlighted / $scope.filteredOrders.length) * 100).toFixed(2) : 0) + '%)';
					}
				}
			}
			catch (e) {
				$scope.errorMsg = 'Error matching orders! Please check syntax in the filter and highlight fields!';
			}
 		};

		/**
		 * Goto a location. 
		 * @param  {[type]} order [description]
		 * @return {[type]}       [description]
		 */
		$scope.orderClick = function(order, $event) {
			var url = ConfigFactory.getOutputName() + '/order/' + order.ticker + '/' + order.id + '/' + ConfigFactory.getSimDataType();
			if ($event && ($event.ctrlKey || $event.shiftKey)) {
				$window.open('#/' + url);
			}
			else {
				$location.url(url);
			}
		};
	} // end controller
]);

