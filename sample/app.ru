# frozen_string_literal: true
require 'bundler/inline'

gemfile(true) do
  source 'https://rubygems.org'
  gem 'vernier'
  gem 'rails'
  gem 'sqlite3'
end

require 'active_record'

# TODO: Use in-memory database
#ENV['DATABASE_URL'] = 'sqlite3::memory:'
#ActiveRecord::Base.establish_connection(adapter: 'sqlite3', database: ':memory:')

ActiveRecord::Base.establish_connection(adapter: 'sqlite3', database: "#{File.dirname(__FILE__)}/db.sqlite3")
ActiveRecord::Base.logger = Logger.new(STDOUT)
ActiveRecord::Schema.define do
  create_table :users, force: true do |t|
    t.string :name
  end

  create_table :articles, force: true do |t|
    t.references :user
    t.string :title
    t.string :body
    t.boolean :published, default: false
  end
end

require 'action_controller/railtie'

class App < Rails::Application
  config.active_support.cache_format_version = 7.0
  config.logger = ActiveSupport::Logger.new($stdout)
  config.eager_load = true
  config.hosts << proc { true } if config.respond_to? :hosts
  config.root = File.dirname(__FILE__)
  config.secret_key_base = SecureRandom.uuid
  config.after_initialize do
    ActiveRecord::Tasks::DatabaseTasks.migrate
  end

  routes.append do
    resources :users, only: %i[create index]
    resources :articles, only: %i[create index]
  end
end

class User < ActiveRecord::Base
  has_many :articles
end

class Article < ActiveRecord::Base
  belongs_to :user
end

class ApplicationController < ActionController::Base
end

class UsersController < ApplicationController
  def index = render json: { users: User.all.count }
  def create = render json: { user: User.create(name: 'User') }
end

class ArticlesController < ApplicationController
  def index = render json: { users: Article.all.count }
  def create = render json: {
    article: Article.create(
      user: User.sample,
      title: 'title',
      body: 'body',
      published: [true, false].sample
    )
  }
end

class ProfilingMiddleware
  def initialize(app)
    require 'fileutils'
    FileUtils.mkdir_p "#{File.dirname(__FILE__)}/profile/"
    @app = app
  end

  def call(env)
    Vernier.start_profile(out: "#{File.dirname(__FILE__)}/profile/#{SecureRandom.uuid}.json")
    status, headers, response = @app.call(env)
    Vernier.stop_profile
    [status, headers, response]
  end
end

App.initialize!

use ProfilingMiddleware
run App
