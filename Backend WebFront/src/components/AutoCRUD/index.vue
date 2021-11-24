<template>
  <div id="div_AutoCRUD">
    <el-row style="background-color: #eee; padding:10px 0px 0px 10px"><!--搜索条件工具条-->
      <el-col>
        <el-form ref="tb_search" :inline="true" :model="filters.filterRules">
          <el-form-item
            v-for="field in ArrSearchField"
            :key="field.Name"
            :label-width="formLabelWidth"
            :label="field.DisplayName"
            :prop="field.Name"
            :rules="el_FormFieldRules(field,true)"
          >
            <component
              :is="el_inputType(field)"
              v-if="!field.IsForeignKey"
              v-model="filters.filterRules[field.Name]"
              :prop="field.Name"
              :type="el_inputProtoType(field,true)"
              :precision="field.Precision"
              value-format="yyyy-MM-dd"
              range-separator="至"
              start-placeholder="日期起"
              end-placeholder="日期讫"
            >
              <i
                v-show="field.Name.toLowerCase().indexOf('password')>=0"
                slot="suffix"
                class="el-input__icon fa"
                :class="el_inputClass(field)"
                @click="pswView(field)"
              />
            </component>
            <el-select
              v-else
              v-model="filters.filterRules[field.Name]"
              reserve-keyword
              clearable
              :remote-method="q=>el_remoteMethod(q,field,'search')"
              :loading="el_selt.el_selt_loading"
              :multiple="field.multiple"
            >
              <template v-if="el_selt[field.Name+'_search']">
                <el-option
                  v-for="item in el_selt[field.Name+'_search'].ArrOption"
                  :key="item.key"
                  :label="item.displayName"
                  :value="item.value"
                />
              </template>
            </el-select>
          </el-form-item>
          <br>
          <el-form-item style="margin-bottom: 8px;">
            <el-button type="primary" icon="el-icon-search" :loading="tbLoading" @click="search">查询</el-button>
          </el-form-item>
          <el-form-item style="margin-bottom: 8px;">
            <el-button icon="el-icon-refresh" :disabled="tbLoading" @click="resetFilter('tb_search')">重置</el-button>
          </el-form-item>
        </el-form>
      </el-col>
    </el-row>
    <el-row style="padding: 3px 10px 3px 10px;"><!--按钮组--><!--padding:上右下左-->
      <el-col>
        <el-button-group>
          <el-button type="primary" icon="el-icon-plus" :disabled="!$route.meta.Create" @click="handleAddRow">新增</el-button>
          <el-button icon="el-icon-download" :disabled="!$route.meta.Export" @click="ExportXls(tableData,'Excel导入配置')">导出</el-button>
          <el-button icon="el-icon-upload" :disabled="!$route.meta.Import" @click="ImportXls">导入</el-button>
        </el-button-group>
      </el-col>
    </el-row>
    <el-row><!--table列表-->
      <el-col>
        <el-table
          ref="Mytb"
          v-loading="tbLoading"
          :data="tableData"
          style="width: 100%"
          max-height="500"
          row-key="Id"
          border
          stripe
          :default-sort="{prop:'Id',order:'descending'}"
          @row-dblclick="handledblclick"
          @selection-change="handleSelectionChange"
          @sort-change="tbSortChange"
        >
          <el-table-column type="selection" width="41" />
          <template>
            <el-table-column
              v-for="field in ArrListField"
              :key="field.Name"
              show-overflow-tooltip
              :width="field.Width_List||120"
              :sortable="field.Sortable?'custom':false"
              :prop="field.Name"
              :label="field.DisplayName"
              :formatter="formatter(field)"
            />
          </template>
          <el-table-column v-if="$route.meta.Delete" fixed="right" label="操作" width="51"><!--table列工具条-->
            <template slot-scope="sp">
              <el-tooltip content="删除" placement="top" effect="light">
                <el-button type="danger" icon="el-icon-delete" circle :disabled="!$route.meta.Delete" @click.native.prevent="deleteRow(sp.$index, sp.row)" />
              </el-tooltip>
            </template>
          </el-table-column>
        </el-table>
        <el-row style="padding-top: 10px;"><!--分页工具条&批量删除-->
          <el-col :span="8">
            <el-button type="danger" :disabled="($route.meta.Delete ? selctRows.length===0 : true)" @click="handledelSeltRow">批量删除</el-button>
          </el-col>
          <el-col :span="16">
            <el-pagination
              v-model="pagiNation"
              style="float:right;"
              :current-page="pagiNation.currentPage"
              :page-sizes="pagiNation.pageSizes"
              :page-size="pagiNation.pageSize"
              :layout="pagiNation.layout"
              :total="pagiNation.total"
              @size-change="pageSizeChange"
              @current-change="pageCurrentChange"
              @prev-click="PrevPage"
              @next-click="NextPage"
            />
          </el-col>
        </el-row>
      </el-col>
    </el-row>
    <!--弹出框v-el-drag-dialog-->
    <el-dialog
      v-if="curr_rowdata !== null && JSON.stringify(curr_rowdata) !== '{}'"
      ref="MyDialog"
      v-loading="dlgLoading"
      width="67%"
      center
      :visible.sync="centerDialogVisible"
      :show-close="false"
      :before-close="(done)=>{dlgClose(done)}"
      :fullscreen="dlgfullscreen"
    >
      <div slot="title" class="el-dialog__title" style="" @dblclick="dlgfullscreen = !dlgfullscreen">
        <el-row>
          <el-col :span="8" style="cursor:move" v-html="'&nbsp;'" />
          <el-col :span="8" style="cursor:move">{{ dgTitle }}</el-col>
          <el-col :span="8" style="text-align:right">
            <el-button type="primary" icon="el-icon-check" :disabled="!$route.meta.Edit" title="确 定" circle @click="dlgSubmit" />
            <el-button type="danger" icon="el-icon-close" title="取 消" circle @click="dlgClose" />
          </el-col>
        </el-row>
      </div>
      <el-form ref="MyForm" :model="curr_rowdata" :label-position="label_position" inline>
        <el-form-item
          v-for="field in ArrFormField"
          :key="field.Name"
          :label-width="formLabelWidth"
          :label="field.DisplayName"
          :prop="field.Name"
          :rules="el_FormFieldRules(field)"
        >
          <component
            :is="el_inputType(field)"
            v-if="!field.IsForeignKey && field.FormShow && field.inputType !== 'tagedit'"
            v-model="curr_rowdata[field.Name]"
            :disabled="field.IsKey || (!field.Editable&&(isNaN(curr_rowdata[keyFieldName])?curr_rowdata[keyFieldName].indexOf('_') < 0:curr_rowdata[keyFieldName]>0))"
            :prop="field.Name"
            :type="el_inputProtoType(field)"
            :precision="field.Precision"
            :clearable="field.Name.toLowerCase().indexOf('password')<0"
            :show-word-limit="(field.MaxLength||50)>0"
            :maxlength="field.MaxLength||50"
            :minlength="field.MinLength||50"
            :style="{'width':(field.Width_input||178)+'px'}"
          >
            <i
              v-if="field.Name.toLowerCase().indexOf('password')>=0"
              v-show="true"
              slot="suffix"
              class="el-input__icon fa"
              :class="el_inputClass(field)"
              @click="pswView(field)"
            />
          </component>
          <component
            :is="el_inputType(field)"
            v-else-if="field.FormShow && field.inputType === 'tagedit'"
            v-model="curr_rowdata[field.Name]"
            :style="{'width':(field.Width_input||378)+'px'}"
            :maxlength="field.MaxLength||20"
            :editable="field.Editable"
          />
          <el-select
            v-else
            v-model="curr_rowdata[field.Name]"
            reserve-keyword
            clearable
            :remote-method="q=>el_remoteMethod(q,field,'form')"
            :loading="el_selt.el_selt_loading"
            :style="{'width':(field.Width_input||178)+'px'}"
            :multiple="field.multiple"
          >
            <template v-if="el_selt[field.Name+'_form']">
              <el-option
                v-for="item in el_selt[field.Name+'_form'].ArrOption"
                :key="item.key"
                :label="item.displayName"
                :value="item.value"
              />
            </template>
          </el-select>
        </el-form-item>
        <el-tabs v-if="ArrTabEditField&&ArrTabEditField.length>0" ref="el_Tab" v-model="TabActiveName" type="border-card" @tab-click="TabClick">
          <template v-for="tab in ArrTabEditField">
            <el-tab-pane :key="tab.Name" :label="tab.DisplayName" :name="tab.Name">
              <AutoCRUDLocal
                v-if="curr_rowdata[tab.Name+'_config'].dlgVisible"
                :ref="tab.Name"
                v-model="curr_rowdata[tab.Name]"
                :ref-field-val="curr_rowdata[keyFieldName]"
                :ref-field-name="fields[tab.Name+'Fields'].refFieldName"
                :del-data="curr_rowdata[tab.Name+'_config'].delData"
                :fields="fields[tab.Name+'Fields']"
                @dlgok_func="dlgOK_Func"
                @updateData="curr_rowdataChange"
              />
              <!-- _self 获取vue实例 filters
              :Fields="tab.Name+'Fields'|getVueDataByStr"
              :Fields="$data[tab.Name+'Fields']"
              :Fields="_self[tab.Name+'Fields']" -->
              <!--keep-alive 保持上次渲染时的状态
              include: 字符串或正则表达式。只有匹配的组件会被缓存。
              exclude: 字符串或正则表达式。任何匹配的组件都不会被缓存。-->
            </el-tab-pane>
          </template>
        </el-tabs><!--TabEdit 子类编辑-->
      </el-form>
      <span slot="footer" class="dialog-footer"><!--底部按钮组-->
        <el-button @click="dlgClose">取 消</el-button>
        <el-button type="primary" :disabled="!$route.meta.Edit" @click="dlgSubmit">确 定</el-button>
      </span>
    </el-dialog>
  </div>
</template>
<script>
import BaseApi from '@/api/BaseApi'
import MycRUDMixin from '@/Mixins/CRUDMixin'
import { objIsEmpty } from '@/utils'
import LazyLoading from '@/components/LazyLoading' // 异步加载

const { cRUDMixin, CustomerFields } = MycRUDMixin

// 自定义列数据(覆盖BaseArrField-ArrField行值)
// CustomerFields.Currency = {
//   Name: 'Currency', //名称
//   DisplayName:  '授权币制',//显示名称
//   IsKey: true, //主键@-默认：false
//   Editable: true, //可编辑-默认：true
//   Required: true, //必填-默认：false
//   Type: 'string', //'datetime/number/string/boolean';//类型-默认: string
//   inputType: 'text', //'password/datetime/text/tagedit/tabedit';//form中的input类型-默认: text
//   multiple: true, //select多选
//   IsForeignKey: true, //外键渲染为Select
//   ForeignKeyGetListUrl: '/api/GetPagerPARA_CURR_FromCache', //获取外键数据Url
//   isEmail: true, //邮件格式
//   FormOrder: 1, //Form排序
//   FormShow: true, //Form中展示-默认：true
//   ListOrder: 1, //列表排序
//   ListShow: true, //列表展示-默认：true
//   MaxLength:  50, //最大长度
//   MinLength: 10, //最小长度
//   Precision: 2 //小数位位数 //Type为number时，可设置小数位
//   SearchOrder: 1, //搜索排序
//   SearchShow: true, //搜索中展示-默认：false
//   Sortable: true, //是否可排序-默认：false
//   Width_List: '120', //列表-列宽度 <=0 默认*，>0 此宽度为准
//   Width_input: '178', //Form-input宽度 <=0 默认*，>0 此宽度为准
// }
// 枚举类型字段
export default {
  name: 'AutoCRUD', // 页面名称（当组件引用时用到）
  components: {
    TagEdit: () => LazyLoading(import('@/components/TagEdit')), // 标签编辑展示
    AutoCRUDLocal: () => LazyLoading(import('@/components/AutoCRUDLocal')) // 本地数据CRUD
  },
  mixins: [cRUDMixin], // 混入
  props: {
    controllerName: { // 控制器名称
      type: String,
      required: true
    },
    customerFields: { // 自定义列数据(覆盖BaseArrField-ArrField行值)
      type: Object,
      required: false,
      default: () => {}
    },
    fields: { // 所有要渲染的字段
      type: Array,
      required: true
    },
    formlabelWidth: {
      type: String,
      default: '120px'
    },
    formlabelPosition: {
      type: String,
      default: 'right'
    },
    title: { // 标题
      type: String,
      default: ''
    }
  },
  created: function() {
    console.log('AutoCrud---------')
    // 赋值渲染然字段
    // ArrEnumField/ArrFormField/ArrListField/ArrSearchField/ArrTagEditField/ArrTabEditField
    // this.fieldsUpdate()
    const { customerFields: propCustomerFields, controllerName } = this
    // 设置自定义数据
    if (!objIsEmpty(propCustomerFields)) {
      propCustomerFields.entries().foreach((key, value) => {
        CustomerFields[key] = value
      })
    }
    // 为了filters能使用this指向vue实例
    this._f = function(id) {
      return this.$options.filters[id].bind(this._self)
    }
    // // 初始化渲染CRUD字段数据
    // BaseArrField.SetArrField = Fields
    // 设置控制器
    BaseApi.SetController(controllerName)
  } // 渲染dom前触发
}
</script>
<style scoped>
  /* 取消用户选择文本 */
  #div_AutoCRUD_ {
    -moz-user-select: none; /*火狐*/
    -webkit-user-select: none; /*webkit浏览器*/
    -ms-user-select: none; /*IE10*/
    -khtml-user-select: none; /*早期浏览器*/
    user-select: none;
  }
</style>
