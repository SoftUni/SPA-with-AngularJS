app.controller('AdsController', function AdsController($scope, $log, adsData) {
	adsData.getAds().then(
		function (data, status, headers, config) {
			$scope.data = data;
		}, 
		function (error, status, headers, config) {
			$log.warn(status, error);
		});
});