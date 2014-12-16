app.controller('NewsController', 
	function NewsController($scope) {
		$scope.sortOrder = 'author';
		$scope.news = {
			name: 'SoftUni is moving to a new office located near the Student city',
			content: '“Software University” Ltd. Our goal in the Software University is to offer quality education, profession and jobs to our students For us, as well as for the software industry, the most important thing is the practical experience. Our mission is to make real professionals out of our students and to help them find a job in the software industry. The curriculum of the Software University consists of a few levels – a preparatory level for the entrance exam (it lasts 2 months) and 6 levels of studying programming, technologies and software development (each of them lasts 4 months). Students have the opportunity to be issued university diploma of higher education by one of our partner universities after additional education (it lasts 1-2 years) according their programs. Who can enroll for the educational program? There are no age, sex or other restrictions. Anyone who is interested can enroll after successfully taking the entrance exam. The Software University team believes in lifelong Learning. Therefore, there are no age restrictions. If the students pass entrance exam, they can enroll for level # 1. Only students over the age of 17 have the right to receive scholarship from the Software University. Only students who have completed their secondary education or about to complete it in this calendar year have the right to enroll in partner universities. Requirements: average knowledge of Bulgarian and English languages average and basic computer literacy. If you are wondering whether your English language is good enough for you to study in the Software University, see the teaching materials in the “C# Basics” course and make sure you understand the English text in them.',
			date: new Date(2014, 10, 12),
			imageUrl: 'https://softuni.bg/Files/UserFiles/ImageGallery/season-may-2015.JPG',
			price: 22.3232432341,
			author: {
				name: 'Petya Grozdarska',
				company: 'Software University',
				picture: 'https://softuni.bg/Users/Profile/ShowAvatar/854e1366-c252-460d-afcd-b4adce092798'
			},
			comments: [
				{
					author: 'Svetlin Nakov',
					content: 'All students are welcome in the new halls',
					date: new Date(2014, 12, 13, 12, 14).toDateString(),
					votes: 0,
				},
				{
					author: 'Vladimir Georgiev',
					content: 'We are very happy to read this article Svetlin',
					date: new Date(2014, 12, 14, 13, 22).toDateString(),
					votes: 0,
				},
				{
					author: 'Atanas Rusenov',
					content: 'Wooohhoooooooooooooooooooo',
					date: new Date(2014, 12, 15, 13, 37).toDateString(),
					votes: 0,
				},
				{
					author: 'Teodor Kurtev',
					content: 'Hurrah, will no longer travel 1 hour to work.',
					date: new Date(2014, 12, 16, 14, 22).toDateString(),
					votes: 0,
				},
				{
					author: 'Vladislav Karamfilov',
					content: 'And I no longer need my car',
					date: new Date(2014, 12, 16, 14, 31).toDateString(),
					votes: 0,
				},
			]
		};

		$scope.upVote = function upVote(comment) {
			comment.votes += 1;
		}

		$scope.downVote = function downVote(comment) {
			comment.votes -= 1;
		}
	});