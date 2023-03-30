const graficoSemanal = () => {
  // Charts
  const chart = document.getElementById("chartContentSemanal").getContext("2d");
  // const labels = Utils.months({ count: 7 });

  const dataDB = [65, 59, 80, 81, 56, 55, 40, 21, 76, 35, 50, 91];

  const data = {
    labels: [
      "Janeiro",
      "Fevereiro",
      "Março",
      "Abril",
      "Maio",
      "Junho",
      "Julho",
      "Agosto",
      "Setembro",
      "Outubro",
      "Novembro",
      "Dezembro",
    ],
    datasets: [
      {
        label: "Relatório semanal",
        data: dataDB,
        fill: true,
        borderColor: "#019aff8f",
        backgroundColor: "#019aff00",
        //   borderColor: () => {
        //     var colors = [];
        //     dataDB.forEach((element) => {
        //       // if (element <= 40) colors.push("rgba(220, 20, 60, 1)");
        //       // else if (element <= 50) colors.push("rgba(255, 166, 0, 1)");
        //       // else if (element <= 70) colors.push("rgba(239, 203, 104, 1)");
        //       // else if (element <= 85) colors.push("rgba(191, 217, 64, 1)");
        //       colors.push("rgba(46, 203, 104, 1)");
        //       // colors.push();
        //     });
        //     return colors;
        //   },
        borderWidth: 5,
        tension: 0.4,
        pointRadius: 5,
        pointHoverRadius: 7,
      },
    ],
  };
  const config = {
    type: "line",
    data: data,
    options: {
      scales: {
        y: {
          ticks: {
            callback: function (value, index, ticks) {
              return value + "%";
            },
          },
        },
      },
      responsive: true,
      plugins: {
        title: {
          font: {
            size: 30,
          },
          display: true,
          text: (ctx) =>
            "Point Style: " + ctx.chart.data.datasets[0].pointStyle,
        },
        legend: {
          labels: {
            font: {
              size: 30,
            },
          },
        },
      },
    },
  };

  new Chart(chart, config);

  function poundData(data) {
    var colors = [];
    data.forEach((element) => {
      // if (element <= 40) colors.push("rgba(220, 20, 60, 0.3)");
      // else if (element <= 50) colors.push("rgba(255, 166, 0, 0.3)");
      // else if (element <= 70) colors.push("rgba(239, 203, 104, 0.3)");
      // else if (element <= 85) colors.push("rgba(191, 217, 64, 0.3)");
      /*  else if (element <= 100) */ colors.push("rgba(46, 203, 104, 0.3)");
      // colors.push();
    });
    return colors;
  }
  // const dados = [30, 40, 40, 45, 87, 99];
};

export default graficoSemanal;
