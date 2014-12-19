app.controller('AdsController', function AdsController($scope, $http, adsData) {
	$http.defaults.headers.common['Authorization'] = 'Bearer ih2sRaLmsnexuCgHDxuy_Z81cZgeMgcftuAWmRplQ2eISXIYf14le4P0Gr25CDNUoG7vvtZp42Bx64znic_A-577IYkhUv_8ybYb7KXFG2uYP9BSLzSuRG5REo6ivg6Jv7_iGU86MOywcYmm25y1gvHsd5PCwp5L28JKtN3CQ-iX2WnuMnjA0sTDh-i2aWA9-dEYjmxv7ITJth1ncBIxDDcO512Q98p7bXjayAdzaQLyntAQTI2VpEOgtHF36sSkKea7QDbOPHKY3JsKZxeVNdq-LMTiIopLiJLUc1SjmweFvj8O73P6lNskUxtisFJdE2BtiPnOjEsO7mdNhWpML-sgQRX3KDQLUwVfjQ5CBk_s15q3Ab21y2uZ_Is4DQNPnoxVWqWG2qby5v-MBB8dBrmgYsJPxZ5PxhuFWx_b0a-GK4lVbfhIRMPxDiaYQJUl_ts_S5Tba5v-bQQ-2CzYA8RM4wpppzqRpapEJ6d2ICo';

	 $scope.data = adsData.getAll();

	// $scope.data = adsData.getById(69);

	// $scope.data = adsData.delete(68);

	// $scope.data = adsData.getById(68);

	// adsData.edit(70, { title: 'Dell Inspiron', text: 'Good laptop' });

	// adsData.create({ title: 'Peugeot 2014', text: 'The car is perfect'});
});