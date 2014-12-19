app.controller('AdsController', function AdsController($scope, $log, adsData) {
	adsData.getAds(
		function (data, status, headers, config) {
			$log.info(status, headers(), config);
			$scope.data = data;
		}, 
		function (error, status, headers, config) {
			$log.error(status, error);
		});
});