﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StockTradingAnalysis.Domain.CQRS.Query.Queries;
using StockTradingAnalysis.Interfaces.Enumerations;
using StockTradingAnalysis.Interfaces.Queries;
using StockTradingAnalysis.Interfaces.Services.Domain;
using StockTradingAnalysis.Web.Common.Interfaces;
using StockTradingAnalysis.Web.Models;

namespace StockTradingAnalysis.Web.Controllers
{
	/// <summary>
	/// The DashboardController gathers all information which is needed to render a dashboard with KPIs.
	/// </summary>
	/// <seealso cref="Controller" />
	public class DashboardController : Controller
	{
		/// <summary>
		/// The query dispatcher
		/// </summary>
		private readonly IQueryDispatcher _queryDispatcher;

		/// <summary>
		/// The accumulation plan statistic service
		/// </summary>
		private readonly IAccumulationPlanStatisticService _accumulationPlanStatisticService;

		/// <summary>
		/// The time slice creation service
		/// </summary>
		private readonly ITimeSliceCreationService _timeSliceCreationService;

		/// <summary>
		/// The statistic card converter repository
		/// </summary>
		private readonly IStatisticCardConverterRepository _statisticCardConverterRepository;

		/// <summary>
		/// The transaction calculation service
		/// </summary>
		private readonly ITransactionCalculationService _transactionCalculationService;

		/// <summary>
		/// Initializes a new instance of the <see cref="DashboardController"/> class.
		/// </summary>
		/// <param name="queryDispatcher">The query dispatcher.</param>
		/// <param name="accumulationPlanStatisticService">The accumulation plan statistic service.</param>
		/// <param name="timeSliceCreationService">The time slice creation service.</param>
		/// <param name="statisticCardConverterRepository">The statistic card converter repository.</param>
		/// <param name="transactionCalculationService">The transaction calculation service.</param>
		public DashboardController(
			IQueryDispatcher queryDispatcher,
			IAccumulationPlanStatisticService accumulationPlanStatisticService,
			ITimeSliceCreationService timeSliceCreationService,
			IStatisticCardConverterRepository statisticCardConverterRepository,
			ITransactionCalculationService transactionCalculationService)
		{
			_queryDispatcher = queryDispatcher;
			_accumulationPlanStatisticService = accumulationPlanStatisticService;
			_timeSliceCreationService = timeSliceCreationService;
			_statisticCardConverterRepository = statisticCardConverterRepository;
			_transactionCalculationService = transactionCalculationService;
		}

		// GET: Dashboard
		public IActionResult Index()
		{
			var model = new DashboardViewModel();

			var statistics =
				_queryDispatcher.Execute(new StatisticsByTimeSliceQuery(_timeSliceCreationService.CreateTimeSlices(), TimeSliceType.All));

			model.Cards = _statisticCardConverterRepository.ConvertStatistic(statistics.FirstOrDefault(s => s.TimeSlice.Type.Equals(TimeSliceType.All)));
			model.OpenPositions = Mapper.Map<OpenPositionsViewModel>(_transactionCalculationService.CalculateOpenPositions());

			return View(model);
		}

		//
		// GET: /Dashboard/SavingsPlan
		public IActionResult SavingsPlan()
		{
			var model = new List<SavingsPlanViewModel>();

			foreach (var tag in _queryDispatcher.Execute(new TransactionTagAllQuery()))
			{
				if (string.IsNullOrEmpty(tag))
					continue;

				model.Add(Mapper.Map<SavingsPlanViewModel>(_accumulationPlanStatisticService.CalculateSavingsPlan(tag)));
			}

			return View(model);
		}

		//
		// GET: /Dashboard/Performance
		public IActionResult Performance()
		{
			var model = new List<PerformanceViewModel>();

			var statistics =
				_queryDispatcher.Execute(new StatisticsByTimeSliceQuery(_timeSliceCreationService.CreateTimeSlices(), TimeSliceType.Year));


			return View(model);
		}
	}
}