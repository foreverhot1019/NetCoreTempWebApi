'use strict'
const path = require('path')
const defaultSettings = require('./src/settings.js')

function resolve(dir) {
  return path.join(__dirname, dir)
}

const name = defaultSettings.title || 'Backend Center' // page title
// 打包分析
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin
const UglifyJsPlugin = require('uglifyjs-webpack-plugin')

// If your port is set to 80,
// use administrator privileges to execute the command line.
// For example, Mac: sudo npm run
// You can change the port by the following method:
// port = 9527 npm run dev OR npm run dev --port = 9527
const port = process.env.port || process.env.npm_config_port || 9527 // dev port

// All configuration item explanations can be find in https://cli.vuejs.org/config/
module.exports = {
  /**
   * You will need to set publicPath if you plan to deploy your site under a sub path,
   * for example GitHub Pages. If you plan to deploy your site to https://foo.github.io/bar/,
   * then publicPath should be set to "/bar/".
   * In most cases please use '/' !!!
   * Detail: https://cli.vuejs.org/config/#publicpath
   */
  publicPath: '/APICenter_BackendCenter/', // 默认'/'，部署应用包时的基本 URL
  outputDir: 'dist', // 'dist', 生产环境构建文件的目录
  assetsDir: 'static', // 相对于outputDir的静态资源(js、css、img、fonts)目录
  filenameHashing: true, // 默认情况下，生成的静态资源在它们的文件名中包含了 hash 以便更好的控制缓存。然而，这也要求 index 的 HTML 是被 Vue CLI 自动生成的。如果你无法使用 Vue CLI 生成的 index HTML，你可以通过将这个选项设为 false 来关闭文件名哈希。
  lintOnSave: process.env.NODE_ENV === 'development', // 保存时eslint检测规范
  runtimeCompiler: true, // 是否使用包含运行时编译器的 Vue 构建版本,可以在 Vue 组件中使用 template
  productionSourceMap: process.env.VUE_APP_SHOW_SOURCE_MAP === 'SHOW_SOURCE_MAP', // 生产环境的 source map
  transpileDependencies: [], // 默认情况下 babel-loader 会忽略所有 node_modules 中的文件,想要Babel转译，请列出
  parallel: require('os').cpus().length > 1,
  pwa: {}, // 生成pwa
  css: { // css相关配置
    // 是否使用css分离插件 ExtractTextPlugin
    // extract: true,
    // 开启 CSS source maps?
    sourceMap: true,
    // css预设器配置项
    loaderOptions: {
      less: { // less允许js
        javascriptEnabled: true
      }
    }
    // 启用 CSS modules for all css / pre-processor files.
    // modules: false
  },
  // multiple-pages 多页面模式下构建应用
  // pages: MutiPageConfig.MutiPageConfig,
  devServer: {
    port: port,
    open: true,
    overlay: {
      warnings: false,
      errors: true
    },
    // before: require('./mock/mock-server.js'),
    // 代理
    proxy: {
      '/dev-api': {
        // target: `https://michaelidssrv.com:44365`, // 设置你调用的接口域名和端口号
        target: `http://localhost:5000`, // 设置你调用的接口域名和端口号
        ws: true,
        changeOrigin: true, // 跨域
        secure: false, // https
        pathRewrite: {
          '^/dev-api/vue-element-admin': '/api',
          '^/dev-api': '/api' // 这里理解成用‘/api’代替target里面的地址，后面组件中我们掉接口时直接用api代替 比如我要调用'http://10.1.5.11:8080/xxx/duty?time=2017-07-07 14:57:22'，直接写‘/api/xxx/duty?time=2017-07-07 14:57:22’即可
        }
      }
    }
  },
  configureWebpack: config => {
    // provide the app's title in webpack's name field, so that
    // it can be accessed in index.html to inject the correct title.
    // name: name,
    // resolve: {
    //   alias: {
    //     '@': resolve('src')
    //   }
    // }
    config.name = name
    // 添加别名
    config.resolve.alias = {
      '@': resolve('src')
    }
    // 设置别名
    // config.resolve.alias['img'] = path.resolve(__dirname, '../src/assets/images');
    // config.resolve.alias['styles'] = path.resolve(__dirname, '../src/assets/styles');
    if (process.env.NODE_ENV !== 'development') { // 为生产环境修改配置...
      // config.mode = 'production'
      // 入口文件
      // config.entry.app = ['./src/main.js'];
      // 删除console插件
      const plugins = [
        new UglifyJsPlugin({
          uglifyOptions: {
            warnings: false,
            compress: {
              drop_console: true,
              drop_debugger: false,
              pure_funcs: ['console.log'] // 移除console
            },
            output: {
              // 去掉注释内容
              comments: false
            }
          },
          sourceMap: process.env.VUE_APP_SHOW_SOURCE_MAP === 'SHOW_SOURCE_MAP',
          parallel: true // 并行化可以显著地加速构建
        })
      ]
      // 重新配置插件
      config.plugins = [...config.plugins, ...plugins]
      if (process.env.NODE_ENV !== 'production' || process.env.VUE_APP_SHOW_SOURCE_MAP === 'SHOW_SOURCE_MAP') {
        config.devtool = 'source-map'
      }
    } else { // 为开发环境修改配置...
      config.mode = 'development'
      config.devtool = 'source-map'
    }
    // 添加 loader
    // config.module.rules.push({
    //   // 处理less
    //   test: /\.less$/,
    //   exclude: /node_modules/,
    //   use: [{
    //     loader:"style-loader",
    //     sourceMap: true
    //   }, {
    //     loader:"css-loader",
    //     sourceMap: true
    //   }, {
    //     loader:"less-loader",
    //     sourceMap: true
    //   }]
    // })
    /* // 添加例外（不打包）
    config.externals = {
      'vue': 'Vue',
      'element-ui': 'ELEMENT',
      'vue-router': 'VueRouter',
      'vuex': 'Vuex',
      'axios': 'axios'
    } */
  },
  chainWebpack(config) {
    // it can improve the speed of the first screen, it is recommended to turn on preload
    // it can improve the speed of the first screen, it is recommended to turn on preload
    config.plugin('preload').tap(() => [
      {
        rel: 'preload',
        // to ignore runtime.js
        // https://github.com/vuejs/vue-cli/blob/dev/packages/@vue/cli-service/lib/config/app.js#L171
        fileBlacklist: [/\.map$/, /hot-update\.js$/, /runtime\..*\.js$/],
        include: 'initial'
      }
    ])

    // 修复.vue中的&nbsp;被自动清除
    config.module
      .rule('vue')
      .use('vue-loader')
      .loader('vue-loader')
      .tap(options => {
        // modify the options...
        options.compilerOptions.preserveWhitespace = true
        return options
      })

    // when there are many pages, it will cause too many meaningless requests
    config.plugins.delete('prefetch')

    // 打包分析
    if (process.env.IS_ANALYZ) {
      config.plugin('webpack-report')
        .use(BundleAnalyzerPlugin, [{
          analyzerMode: 'static'
        }])
    }

    // set svg-sprite-loader
    config.module
      .rule('svg')
      .exclude.add(resolve('src/icons'))
      .end()
    config.module
      .rule('icons')
      .test(/\.svg$/)
      .include.add(resolve('src/icons'))
      .end()
      .use('svg-sprite-loader')
      .loader('svg-sprite-loader')
      .options({
        symbolId: 'icon-[name]'
      })
      .end()

    // https://webpack.js.org/configuration/devtool/#development
    config.when(process.env.NODE_ENV !== 'production', config => {
      config.devtool('cheap-source-map')
    })
    config
      .when(process.env.NODE_ENV !== 'development',
        config => {
          config
            .plugin('ScriptExtHtmlWebpackPlugin')
            .after('html')
            .use('script-ext-html-webpack-plugin', [{
            // `runtime` must same as runtimeChunk name. default is `runtime`
              inline: /runtime\..*\.js$/
            }])
            .end()
          config
            .optimization.splitChunks({
              chunks: 'all',
              cacheGroups: {
                libs: {
                  name: 'chunk-libs',
                  test: /[\\/]node_modules[\\/]/,
                  priority: 10,
                  chunks: 'initial' // only package third parties that are initially dependent
                },
                elementUI: {
                  name: 'chunk-elementUI', // split elementUI into a single package
                  priority: 20, // the weight needs to be larger than libs and app or it will be packaged into libs or app
                  test: /[\\/]node_modules[\\/]_?element-ui(.*)/ // in order to adapt to cnpm
                },
                commons: {
                  name: 'chunk-commons',
                  test: resolve('src/components'), // can customize your rules
                  minChunks: 3, //  minimum common number
                  priority: 5,
                  reuseExistingChunk: true
                }
              }
            })
          // https:// webpack.js.org/configuration/optimization/#optimizationruntimechunk
          config.optimization.runtimeChunk('single')
        }
      )
  }
}
